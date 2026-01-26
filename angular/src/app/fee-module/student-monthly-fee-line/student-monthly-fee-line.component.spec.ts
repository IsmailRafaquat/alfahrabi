import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentMonthlyFeeLineComponent } from './student-monthly-fee-line.component';

describe('StudentMonthlyFeeLineComponent', () => {
  let component: StudentMonthlyFeeLineComponent;
  let fixture: ComponentFixture<StudentMonthlyFeeLineComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StudentMonthlyFeeLineComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StudentMonthlyFeeLineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
