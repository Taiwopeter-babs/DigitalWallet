import { NODE_ENV } from "./common";

export function getBaseApiUrl() {
  let url = process.env.API_URL;

  if (url) return url;

  return "http://localhost:5001";
}
