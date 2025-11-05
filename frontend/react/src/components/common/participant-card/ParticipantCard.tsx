import CopyButton from "../copy-button/CopyButton";
import InfoButton from "../info-button/InfoButton";
import DeleteButton from "../delete-button/DeleteButton";
import ItemCard from "../item-card/ItemCard";
import { type ParticipantCardProps } from "./types";
import "./ParticipantCard.scss";

const ParticipantCard = ({
  firstName,
  lastName,
  isCurrentUser = false,
  isAdmin = false,
  isCurrentUserAdmin = false,
  adminInfo = "",
  participantLink = "",
  onInfoButtonClick,
  onDeleteButtonClick,
}: ParticipantCardProps) => {
  return (
    <ItemCard title={`${firstName} ${lastName}`} isFocusable>
      <div className="participant-card-info-container">
        {isCurrentUser ? <p className="participant-card-role">You</p> : null}

        {!isCurrentUser && isAdmin ? (
          <p className="participant-card-role">Admin</p>
        ) : null}

        <div className="participant-card__link">
          {isCurrentUserAdmin ? (
            <CopyButton
              textToCopy={participantLink}
              iconName="link"
              successMessage="Personal Link is copied!"
              errorMessage="Personal Link was not copied. Try again."
            />
          ) : null}
        </div>

        <div className="participant-card__actions">
          {isCurrentUserAdmin && !isAdmin ? (
            <InfoButton withoutToaster onClick={onInfoButtonClick} />
          ) : null}

          {isCurrentUserAdmin && !isCurrentUser && !isAdmin ? (
            <DeleteButton onClick={onDeleteButtonClick} />
          ) : null}

          {!isCurrentUser && isAdmin ? (
            <InfoButton infoMessage={adminInfo} />
          ) : null}
        </div>
      </div>
    </ItemCard>
  );
};

export default ParticipantCard;
