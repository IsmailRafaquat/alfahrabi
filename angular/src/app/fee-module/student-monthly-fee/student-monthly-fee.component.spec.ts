import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentMonthlyFeeComponent } from './student-monthly-fee.component';

describe('StudentMonthlyFeeComponent', () => {
  let component: StudentMonthlyFeeComponent;
  let fixture: ComponentFixture<StudentMonthlyFeeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StudentMonthlyFeeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StudentMonthlyFeeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
