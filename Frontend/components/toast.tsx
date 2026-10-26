import { Check, CircleAlert, X } from "lucide-react";

type ToastProps = {
  notice: {
    kind: "success" | "error";
    message: string;
  };
  onDismiss: () => void;
};

export function Toast({ notice, onDismiss }: ToastProps) {
  return (
    <div className={`toast toast-${notice.kind}`} role={notice.kind === "error" ? "alert" : "status"}>
      <span className="toast-icon">{notice.kind === "success" ? <Check size={17} /> : <CircleAlert size={17} />}</span>
      <span>{notice.message}</span>
      <button type="button" onClick={onDismiss} aria-label="Dismiss notification"><X size={16} /></button>
    </div>
  );
}
