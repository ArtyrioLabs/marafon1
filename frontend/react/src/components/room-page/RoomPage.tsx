import { useEffect } from "react";
import { useParams } from "react-router";
import type {
  GetParticipantsResponse,
  GetRoomResponse,
  DrawRoomResponse,
} from "@types/api.ts";
import Loader from "@components/common/loader/Loader.tsx";
import { useFetch } from "@hooks/useFetch.ts";
import useToaster from "@hooks/useToaster.ts";
import { BASE_API_URL } from "@utils/general.ts";
import RoomPageContent from "./room-page-content/RoomPageContent.tsx";
import { ROOM_PAGE_TITLE } from "./utils.ts";
import "./RoomPage.scss";

const RoomPage = () => {
  const { showToast } = useToaster();
  const { userCode } = useParams();

  useEffect(() => {
    document.title = ROOM_PAGE_TITLE;
  }, []);

  const {
    data: roomDetails,
    isLoading: isLoadingRoomDetails,
    fetchData: fetchRoomDetails,
  } = useFetch<GetRoomResponse>(
    {
      url: `${BASE_API_URL}/api/rooms?userCode=${userCode}`,
      method: "GET",
      headers: { "Content-Type": "application/json" },
      onError: () => {
        showToast("Something went wrong. Try again.", "error", "large");
      },
    },
    true,
  );

  const {
    data: participants,
    isLoading: isLoadingParticipants,
    fetchData: fetchParticipants,
  } = useFetch<GetParticipantsResponse>({
    url: `${BASE_API_URL}/api/users?userCode=${userCode}`,
    method: "GET",
    headers: { "Content-Type": "application/json" },
    onError: () => {
      showToast("Something went wrong. Try again.", "error", "large");
    },
  });

  const { fetchData: fetchRandomize, isLoading: isRandomizing } =
    useFetch<DrawRoomResponse>(
      {
        url: `${BASE_API_URL}/api/rooms/draw?userCode=${userCode}`,
        method: "POST",
        headers: { "Content-Type": "application/json" },
        onSuccess: () => {
          showToast(
            "Success! All participants are matched.\nLet the gifting magic start!",
            "success",
            "large",
          );
          fetchRoomDetails();
          fetchParticipants();
        },
        onError: () => {
          showToast("Something went wrong. Try again.", "error", "large");
        },
      },
      false,
    );

  const handleDeleteUser = async (userId: number) => {
    console.log("Attempting to delete user:", userId, "with userCode:", userCode);
    console.log("URL:", `${BASE_API_URL}/api/users/${userId}?userCode=${userCode}`);
    
    try {
      const response = await fetch(
        `${BASE_API_URL}/api/users/${userId}?userCode=${userCode}`,
        {
          method: "DELETE",
          headers: { "Content-Type": "application/json" },
        }
      );

      console.log("Response status:", response.status);
      console.log("Response ok:", response.ok);

      if (response.ok) {
        showToast("User has been successfully removed", "success", "large");
        fetchParticipants();
      } else {
        const errorText = await response.text();
        console.error("Delete failed:", response.status, errorText);
        showToast("Failed to delete user. Try again.", "error", "large");
      }
    } catch (error) {
      console.error("Delete error:", error);
      showToast("Failed to delete user. Try again.", "error", "large");
    }
  };

  const isLoading =
    isLoadingRoomDetails || isLoadingParticipants || isRandomizing;

  if (!userCode) {
    return null;
  }

  return (
    <main className="room-page">
      {isLoading ? <Loader /> : null}

      <RoomPageContent
        participants={participants ?? []}
        roomDetails={roomDetails ?? ({} as GetRoomResponse)}
        onDrawNames={() => fetchRandomize()}
        onDeleteUser={handleDeleteUser}
      />
    </main>
  );
};

export default RoomPage;
