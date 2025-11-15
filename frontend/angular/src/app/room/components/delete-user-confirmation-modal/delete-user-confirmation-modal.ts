import { Component, computed, input, output } from '@angular/core';

import { CommonModalTemplate } from '../../../shared/components/modal/common-modal-template/common-modal-template';
import {
  ButtonText,
  ModalSubtitle,
  ModalTitle,
  PictureName,
} from '../../../app.enum';

@Component({
  selector: 'app-delete-user-confirmation-modal',
  imports: [CommonModalTemplate],
  templateUrl: './delete-user-confirmation-modal.html',
  styleUrl: './delete-user-confirmation-modal.scss',
})
export class DeleteUserConfirmationModal {
  readonly userName = input.required<string>();

  readonly closeModal = output<void>();
  readonly buttonAction = output<void>();
  readonly cancelButtonAction = output<void>();

  public readonly pictureName = PictureName.Cookie;
  public readonly title = ModalTitle.DeleteUser;
  public readonly subtitle = computed(() => `${ModalSubtitle.DeleteConfirmation} ${this.userName()}?`);
  public readonly buttonText = ButtonText.Delete;
  public readonly cancelButtonText = ButtonText.Cancel;

  public onCloseModal(): void {
    this.closeModal.emit();
  }

  public onConfirmDelete(): void {
    this.buttonAction.emit();
  }

  public onCancel(): void {
    this.cancelButtonAction.emit();
  }
}

