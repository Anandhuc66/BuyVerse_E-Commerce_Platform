import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="popup-container">
      <div class="icon-wrapper">
        <i class="bi" [ngClass]="data.icon || 'bi-exclamation-triangle-fill'" style="font-size:32px; color:var(--button);"></i>
      </div>
      <h2 class="title">{{ data.title || 'Confirm' }}</h2>
      <p class="subtitle">{{ data.message || 'Are you sure?' }}</p>
      <div class="btn-group">
        <button class="btn-confirm" (click)="onConfirm()">{{ data.confirmText || 'Confirm' }}</button>
        <button class="btn-cancel" (click)="onCancel()">{{ data.cancelText || 'Cancel' }}</button>
      </div>
    </div>
  `,
  styles: [`
    .popup-container { text-align: center; padding: 25px 20px; }
    .icon-wrapper { background: #e8f0fe; width: 70px; height: 70px; margin: auto; border-radius: 50%; display: flex; align-items: center; justify-content: center; }
    .title { font-size: 22px; font-weight: bold; margin-top: 15px; color: #333; }
    .subtitle { font-size: 14px; color: #6c757d; margin: 10px 0 20px; }
    .btn-group { display: flex; justify-content: center; gap: 15px; margin-top: 10px; }
    .btn-confirm { background-color: var(--button); color: white; padding: 10px 18px; border-radius: 6px; border: none; cursor: pointer; transition: 0.3s; }
    .btn-confirm:hover { background-color: var(--button-hover); }
    .btn-cancel { background-color: #f1f1f1; color: #444; padding: 10px 18px; border-radius: 6px; border: none; cursor: pointer; transition: 0.3s; }
    .btn-cancel:hover { background-color: #e0e0e0; }
  `]
})
export class ConfirmDialog {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { title?: string; message?: string; icon?: string; confirmText?: string; cancelText?: string }
  ) {}

  onConfirm() {
    this.dialogRef.close(true);
  }

  onCancel() {
    this.dialogRef.close(false);
  }
}
