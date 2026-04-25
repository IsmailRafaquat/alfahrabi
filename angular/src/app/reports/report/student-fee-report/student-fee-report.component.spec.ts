import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentFeeReportComponent } from './student-fee-report.component';

describe('StudentFeeReportComponent', () => {
  let component: StudentFeeReportComponent;
  let fixture: ComponentFixture<StudentFeeReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StudentFeeReportComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StudentFeeReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
