import React, { useState, useEffect } from "react";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import { FaHeart, FaCommentAlt } from "react-icons/fa";
import CommentThread from "../components/comments/CommentThread";
import useCategoriesMap from "../hooks/useCategoriesMap";
import { deletePost, toggleLikePost } from "../api/postApi";
import { getUserById } from "../api/userApi";
import { useAuth } from "../context/AuthContext";

dayjs.extend(relativeTime);

export default function PostView({ post, onEdit, onDelete }) {
  const { map: categoriesMap } = useCategoriesMap();
  const { user: currentUser } = useAuth();
  const [showComments, setShowComments] = useState(false);
  const [currentMediaIndex, setCurrentMediaIndex] = useState(0);
  const [commentsCount, setCommentsCount] = useState(0);
  const [likedByUserIds, setLikedByUserIds] = useState(post?.likedByUserIds || []);

  const [author, setAuthor] = useState(null);
  const [loadingAuthor, setLoadingAuthor] = useState(true);
  const [authorError, setAuthorError] = useState(null);

  const mediaUrls = Array.isArray(post?.mediaUrls) ? post.mediaUrls : [];
  const tagsIds = Array.isArray(post?.tagsIds) ? post.tagsIds : [];

  const isEdited = post?.updatedAt && post?.updatedAt !== post?.createdAt;

  const canEditOrDelete =
    currentUser &&
    (String(currentUser.id) === String(post.authorId) || currentUser.role === "admin");

  useEffect(() => {
    const fetchAuthor = async () => {
      if (!post?.authorId) {
        setLoadingAuthor(false);
        return;
      }
      try {
        setLoadingAuthor(true);
        const user = await getUserById(post.authorId);
        setAuthor(user);
      } catch (err) {
        console.error("Greška pri učitavanju autora:", err);
        setAuthorError("Ne mogu da učitam podatke o autoru.");
      } finally {
        setLoadingAuthor(false);
      }
    };
    fetchAuthor();
  }, [post?.authorId]);

  const handleToggleComments = () => setShowComments(!showComments);

  const handlePrevMedia = () => {
    if (!mediaUrls.length) return;
    setCurrentMediaIndex((prev) => (prev > 0 ? prev - 1 : mediaUrls.length - 1));
  };

  const handleNextMedia = () => {
    if (!mediaUrls.length) return;
    setCurrentMediaIndex((prev) => (prev < mediaUrls.length - 1 ? prev + 1 : 0));
  };

  const handleDelete = async () => {
    if (!window.confirm("Da li ste sigurni da želite da obrišete ovaj post?")) return;
    try {
      await deletePost(post.id);
      onDelete?.(post.id);
    } catch (err) {
      console.error(err);
      alert("Greška pri brisanju posta.");
    }
  };

  const handleLike = async () => {
    if (!currentUser) return;
    try {
      const res = await toggleLikePost(post.id, currentUser.id);
      setLikedByUserIds(res.data.likedByUserIds);
    } catch (err) {
      console.error("Greška pri lajkovanju:", err);
    }
  };

  if (loadingAuthor)
    return <div className="p-8 text-gray-500 text-center font-semibold">Učitavanje autora...</div>;
  if (authorError)
    return <div className="p-8 text-red-500 text-center font-semibold">{authorError}</div>;

  const currentMedia = mediaUrls[currentMediaIndex] || null;
  const likedByCurrentUser = likedByUserIds.includes(currentUser?.id);

  return (
    <div className="bg-zinc-950 rounded-3xl shadow-2xl border border-zinc-800 mb-8 transform transition-all duration-500 hover:shadow-yellow-500/10">
      
      {/* Zaglavlje */}
      <div className="flex items-center justify-between p-6 border-b border-zinc-800">
        <div className="flex items-center gap-4">
          <img
            src={author?.avatarUrl ?? "https://placehold.co/56x56/1e293b/d4d4d8?text=U"}
            alt="Avatar"
            className="w-16 h-16 rounded-full object-cover border-4 border-yellow-500 shadow-lg"
          />
          <div>
            <p className="font-bold text-2xl text-white">{author?.username ?? "Anonymous"}</p>
            <p className="text-sm text-gray-400">
              {post?.createdAt ? dayjs(post.createdAt).fromNow() : "Nepoznat datum"}
              {isEdited && post?.updatedAt && (
                <span className="ml-2 text-gray-500">(Izmenjeno)</span>
              )}
            </p>
          </div>
        </div>

        {canEditOrDelete && (
          <div className="flex gap-2">
            <button
              onClick={() => onEdit?.(post)}
              className="px-4 py-2 rounded-full bg-yellow-600 text-white font-semibold hover:bg-yellow-500 transition-colors"
            >
              Izmeni
            </button>
            <button
              onClick={handleDelete}
              className="px-4 py-2 rounded-full bg-red-600 text-white font-semibold hover:bg-red-500 transition-colors"
            >
              Obriši
            </button>
          </div>
        )}
      </div>

      {/* Sadržaj */}
      <div className="p-6 flex flex-col gap-6">
        <h2 className="text-4xl font-extrabold text-red-500 leading-tight">{post?.title ?? "Untitled Post"}</h2>

        {tagsIds.length > 0 && (
          <div className="flex flex-wrap gap-2">
            {tagsIds.map((tagId, i) => (
              <span
                key={i}
                className="text-sm px-4 py-1.5 rounded-full bg-zinc-800 text-gray-400 font-medium border border-zinc-700 transition-colors hover:bg-yellow-600 hover:text-white hover:border-yellow-600 cursor-pointer"
              >
                #{categoriesMap[tagId] ?? tagId}
              </span>
            ))}
          </div>
        )}

        <p className="text-lg text-gray-300 whitespace-pre-wrap">{post?.body ?? ""}</p>

        {currentMedia && (
          <div className="relative w-full aspect-video rounded-3xl overflow-hidden shadow-lg border border-zinc-700">
            {mediaUrls.length > 1 && (
              <>
                <button
                  onClick={handlePrevMedia}
                  className="absolute left-4 top-1/2 -translate-y-1/2 bg-black bg-opacity-50 text-white w-14 h-14 rounded-full flex justify-center items-center text-3xl hover:bg-opacity-80 transition-all z-10 focus:outline-none"
                >
                  ❮
                </button>
                <button
                  onClick={handleNextMedia}
                  className="absolute right-4 top-1/2 -translate-y-1/2 bg-black bg-opacity-50 text-white w-14 h-14 rounded-full flex justify-center items-center text-3xl hover:bg-opacity-80 transition-all z-10 focus:outline-none"
                >
                  ❯
                </button>
                <div className="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2 z-10">
                  {mediaUrls.map((_, idx) => (
                    <div
                      key={idx}
                      className={`w-3 h-3 rounded-full transition-colors ${
                        idx === currentMediaIndex ? "bg-yellow-500" : "bg-gray-400 opacity-50"
                      }`}
                    />
                  ))}
                </div>
              </>
            )}

            {(() => {
              try {
                const parsedUrl = new URL(currentMedia);
                if (parsedUrl.hostname.includes("youtube.com") || parsedUrl.hostname.includes("youtu.be")) {
                  const videoId = parsedUrl.hostname.includes("youtube.com")
                    ? parsedUrl.searchParams.get("v")
                    : parsedUrl.pathname.slice(1);
                  return (
                    <iframe
                      key={currentMediaIndex}
                      src={`https://www.youtube.com/embed/${videoId}?rel=0`}
                      title="YouTube video player"
                      className="w-full h-full"
                      allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                      allowFullScreen
                    ></iframe>
                  );
                }
              } catch {}
              return (
                <img
                  key={currentMediaIndex}
                  src={currentMedia}
                  alt="media preview"
                  className="w-full h-full object-contain bg-zinc-800"
                  onError={(e) => {
                    e.target.onerror = null;
                    e.target.src = "https://placehold.co/720x405/333333/ffffff?text=Video+nedostupan";
                  }}
                />
              );
            })()}
          </div>
        )}
      </div>

      {/* Akcije */}
      <div className="flex items-center gap-8 px-6 py-4 border-t border-zinc-800">
        <button
          className={`flex items-center gap-2 text-xl transition-colors ${
            likedByCurrentUser ? "text-red-500" : "text-gray-400 hover:text-red-500"
          }`}
          onClick={handleLike}
        >
          <FaHeart className="text-3xl" />
          <span className="text-xl font-bold">{likedByUserIds.length}</span>
        </button>

        <button
          className="flex items-center gap-2 text-gray-400 hover:text-white transition-colors animate-pulse"
          onClick={handleToggleComments}
        >
          <FaCommentAlt className="text-3xl" />
          <span className="text-xl font-bold">
            Komentari {commentsCount ? `(${commentsCount})` : ""}
          </span>
        </button>
      </div>

      {/* Komentari */}
      {showComments && (
        <div className="p-6 bg-zinc-900 border-t border-zinc-800 rounded-b-3xl shadow-inner transition-all duration-300 ease-in-out text-white">
          {/* Samo CommentThread, bez dodatnog input polja */}
          <CommentThread postId={post.id} onCountChange={setCommentsCount} className="w-full p-3 rounded-xl placeholder-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500" />
        </div>
      )}
    </div>
  );
}