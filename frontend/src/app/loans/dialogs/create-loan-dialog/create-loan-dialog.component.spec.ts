import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { CreateLoanDialogComponent } from './create-loan-dialog.component';

describe('CreateLoanDialogComponent', () => {
  let component: CreateLoanDialogComponent;
  let fixture: ComponentFixture<CreateLoanDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CreateLoanDialogComponent,
        NoopAnimationsModule,
      ],
      providers: [
        {
          provide: MatDialogRef,
          useValue: {
            close: jasmine.createSpy('close'),
            disableClose: false,
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateLoanDialogComponent);
    component = fixture.componentInstance;
  });

  it('starts numeric money and rate fields as null', () => {
    expect(component.form.controls.principalAmount.value).toBeNull();
    expect(component.form.controls.annualInterestRate.value).toBeNull();
    expect(component.form.controls.currentBalance.value).toBeNull();
  });

  it('blocks submit when required fields are invalid', () => {
    spyOn(component.createRequested, 'emit');

    component.submit();

    expect(component.createRequested.emit).not.toHaveBeenCalled();
    expect(component.form.touched).toBeTrue();
  });

  it('emits the expected payload when valid', () => {
    spyOn(component.createRequested, 'emit');

    component.form.setValue({
      applicantName: ' Ada Lovelace ',
      applicantEmail: 'ada@example.com',
      principalAmount: 10000,
      annualInterestRate: 6.5,
      termMonths: 36,
      currentBalance: 9500,
    });

    component.submit();

    expect(component.createRequested.emit).toHaveBeenCalledOnceWith({
      applicantName: 'Ada Lovelace',
      applicantEmail: 'ada@example.com',
      principalAmount: 10000,
      annualInterestRate: 6.5,
      termMonths: 36,
      currentBalance: 9500,
    });
  });
});
