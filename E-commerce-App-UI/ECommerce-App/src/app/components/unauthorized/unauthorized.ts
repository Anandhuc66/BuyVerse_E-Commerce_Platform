import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Router } from '@angular/router';
@Component({
  selector: 'app-unauthorized',
  standalone: false,
  templateUrl: './unauthorized.html',
  styleUrl: './unauthorized.css'
})
export class Unauthorized {
  constructor(
    @Inject(MatDialogRef) public dialogRef: MatDialogRef<Unauthorized>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private router:Router
  ) {}

   goToLogin() {
    this.dialogRef.close();
    this.router.navigate(['/login']);
  }
  close() {
    this.dialogRef.close();
  }
}
