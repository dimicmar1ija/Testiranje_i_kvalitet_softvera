import { useEffect, useState } from "react";
import http from "../api/axiosInstance"; 

export default function useCategoriesMap() {
  const [map, setMap] = useState({});
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        const data = await http.get("/category").then(r => r.data);
        const m = {};
        for (const c of data) m[c.id] = c.name;
        setMap(m);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  return { map, loading };
}
