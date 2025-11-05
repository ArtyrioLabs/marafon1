import { useRef } from "react";
import IconButton from "../icon-button/IconButton";
import Toaster from "../toaster/Toaster";
import type { ToasterHandler } from "../toaster/types";
import type { DeleteButtonProps } from "./types";
import "./DeleteButton.scss";

const DeleteButton = ({
  onClick,
  disabled = false,
}: DeleteButtonProps) => {
  const toasterRef = useRef<ToasterHandler>(null);

  const handleClick = () => {
    onClick?.();
  };

  return (
    <div className="delete-button">
      <IconButton
        iconName="delete"
        color="green"
        onClick={handleClick}
        disabled={disabled}
      />
      <Toaster ref={toasterRef} isSticky />
    </div>
  );
};

export default DeleteButton;
