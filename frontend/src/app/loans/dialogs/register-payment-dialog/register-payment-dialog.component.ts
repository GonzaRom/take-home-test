import { CurrencyPipe } from '@angular/common';
import { Component, EventEmitter, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { LoanPaymentRequest } from '../../models/loan-payment-request';
import { LoanSummary } from '../../models/loan-summary';

export interface RegisterPaymentDialogData {
  readonly loan: LoanSummary;
}

@Component({
  selector: 'app-register-payment-dialog',
  standalone: true,
  imports: [
    CurrencyPipe,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
  ],
  templateUrl: './register-payment-dialog.component.html',
  styleUrl: './register-payment-dialog.component.scss',
})
export class RegisterPaymentDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject<MatDialogRef<RegisterPaymentDialogComponent, boolean>>(MatDialogRef);

  readonly data = inject<RegisterPaymentDialogData>(MAT_DIALOG_DATA);
  readonly paymentRequested = new EventEmitter<LoanPaymentRequest>();

  readonly form = this.formBuilder.group({
    amount: this.formBuilder.control<number | null>(
      null,
      [
        Validators.required,
        Validators.min(0.01),
        paymentAmountNotAboveBalanceValidator(this.data.loan.currentBalance),
      ],
    ),
    note: this.formBuilder.nonNullable.control('', [Validators.maxLength(500)]),
  });

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
    const amount = formValue.amount;

    if (amount === null) {
      this.form.markAllAsTouched();
      return;
    }

    const note = formValue.note.trim();
    const request: LoanPaymentRequest = {
      amount: Number(amount),
    };

    if (note.length > 0) {
      request.note = note;
    }

    this.submitError = null;
    this.paymentRequested.emit(request);
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

function paymentAmountNotAboveBalanceValidator(currentBalance: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const amount = Number(control.value);

    if (!Number.isFinite(amount) || amount <= 0) {
      return null;
    }

    return amount > currentBalance ? { paymentExceedsBalance: true } : null;
  };
}
