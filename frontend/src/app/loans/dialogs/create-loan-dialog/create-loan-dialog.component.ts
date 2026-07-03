import { Component, EventEmitter, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { CreateLoanRequest } from '../../models/create-loan-request';

@Component({
  selector: 'app-create-loan-dialog',
  standalone: true,
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
  ],
  templateUrl: './create-loan-dialog.component.html',
  styleUrl: './create-loan-dialog.component.scss',
})
export class CreateLoanDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject<MatDialogRef<CreateLoanDialogComponent, boolean>>(MatDialogRef);

  readonly createRequested = new EventEmitter<CreateLoanRequest>();

  readonly form = this.formBuilder.group(
    {
      applicantName: this.formBuilder.nonNullable.control('', [
        Validators.required,
        Validators.maxLength(200),
      ]),
      applicantEmail: this.formBuilder.nonNullable.control('', [
        Validators.required,
        Validators.email,
        Validators.maxLength(320),
      ]),
      principalAmount: this.formBuilder.control<number | null>(null, [
        Validators.required,
        Validators.min(0.01),
      ]),
      annualInterestRate: this.formBuilder.control<number | null>(null, [
        Validators.required,
        Validators.min(0),
      ]),
      termMonths: this.formBuilder.nonNullable.control(12, [
        Validators.required,
        Validators.min(1),
        Validators.max(360),
      ]),
      currentBalance: this.formBuilder.control<number | null>(null, [
        Validators.required,
        Validators.min(0),
      ]),
    },
    { validators: currentBalanceWithinPrincipalValidator },
  );

  isSubmitting = false;
  submitError: string | null = null;

  submit(): void {
    if (this.isSubmitting) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const formValue = this.form.getRawValue();
    const principalAmount = formValue.principalAmount;
    const annualInterestRate = formValue.annualInterestRate;
    const currentBalance = formValue.currentBalance;

    if (principalAmount === null || annualInterestRate === null || currentBalance === null) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitError = null;
    this.createRequested.emit({
      applicantName: formValue.applicantName.trim(),
      applicantEmail: formValue.applicantEmail.trim(),
      principalAmount: Number(principalAmount),
      annualInterestRate: Number(annualInterestRate),
      termMonths: Number(formValue.termMonths),
      currentBalance: Number(currentBalance),
    });
  }

  cancel(): void {
    if (!this.isSubmitting) {
      this.dialogRef.close(false);
    }
  }

  startSubmit(): void {
    this.isSubmitting = true;
    this.submitError = null;
    this.dialogRef.disableClose = true;
    this.form.disable();
  }

  finishSubmitWithError(message: string): void {
    this.isSubmitting = false;
    this.submitError = message;
    this.dialogRef.disableClose = false;
    this.form.enable();
  }
}

function currentBalanceWithinPrincipalValidator(control: AbstractControl): ValidationErrors | null {
  const principalAmountValue = control.get('principalAmount')?.value;
  const currentBalanceValue = control.get('currentBalance')?.value;

  if (principalAmountValue === null || currentBalanceValue === null) {
    return null;
  }

  const principalAmount = Number(principalAmountValue);
  const currentBalance = Number(currentBalanceValue);

  if (!Number.isFinite(principalAmount) || !Number.isFinite(currentBalance)) {
    return null;
  }

  return currentBalance > principalAmount ? { currentBalanceExceedsPrincipal: true } : null;
}
