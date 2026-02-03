import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StaffSalaryPaymentComponent } from './staff-salary-payment.component';

describe('StaffSalaryPaymentComponent', () => {
  let component: StaffSalaryPaymentComponent;
  let fixture: ComponentFixture<StaffSalaryPaymentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StaffSalaryPaymentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StaffSalaryPaymentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
