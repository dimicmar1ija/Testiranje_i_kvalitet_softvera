export function getSubFromToken(token) {
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload?.sub ?? null; // backend stavlja user.Id u "sub"
  } catch {
    return null;
  }
}
