import { useEffect, useState, useContext } from "react";
import CommentForm from "./CommentForm";
import CommentItem from "./CommentItem";
import {
  getThreadedByPost,
  createComment,
  updateComment,
  deleteComment,
  likeComment,
  dislikeComment,
  unlikeComment,
  undislikeComment,
} from "../../api/commentApi";
import { AuthContext } from "../../context/AuthContext";
import { getSubFromToken } from "../../utils/jwt";

export default function CommentThread({ postId, onCountChange }) {
  const [tree, setTree] = useState([]);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState(null);

  const { token } = useContext(AuthContext);
  const currentUserId = getSubFromToken(token);

  const countDeep = (nodes) =>
    (nodes || []).reduce(
      (acc, n) => acc + 1 + (n.replies ? countDeep(n.replies) : 0),
      0
    );

  const refresh = async () => {
    setLoading(true);
    setErr(null);
    try {
      const data = await getThreadedByPost(postId);
      setTree(data);
      onCountChange?.(countDeep(data)); 
    } catch {
      setErr("Ne mogu da učitam komentare.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { if (postId) refresh(); }, [postId]);

  const onCreate = async ({ postId, body, parentCommentId }) => {
    await createComment({ postId, body, parentCommentId, authorId: currentUserId });
    await refresh();
  };

  const onEdit = async (id, payload) => {
    await updateComment(id, payload);
    await refresh();
  };

  const onDelete = async (id) => {
    console.log('token=', token)
    await deleteComment(id, token);
    await refresh();
  };

  const withUser = (fn) => async (id) => {
    if (!currentUserId) return;
    await fn(id, currentUserId);
    await refresh();
  };

  if (loading) return <div>Učitavanje komentara…</div>;
  if (err) return <div style={{ color: "#e11d48" }}>{err}</div>;

  return (
    <div style={{ textAlign: "left", display: "grid", gap: 12 }}>
      <h3>Komentari</h3>
      <CommentForm postId={postId} onSubmit={onCreate} />

      <div>
        {tree?.length === 0 && <div style={{ fontSize: 14, color: "#666" }}>Još nema komentara.</div>}
        {tree?.map((node) => (
          <CommentItem
            key={node.comment.id}
            node={node}
            currentUserId={currentUserId}
            onReply={onCreate}
            onEdit={onEdit}
            onDelete={onDelete}
            onLike={withUser(likeComment)}
            onDislike={withUser(dislikeComment)}
            onUnlike={withUser(unlikeComment)}
            onUndislike={withUser(undislikeComment)}
          />
        ))}
      </div>
    </div>
  );
}
