import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentAttendanceInsightsComponent } from './student-attendance-insights.component';

describe('StudentAttendanceInsightsComponent', () => {
  let component: StudentAttendanceInsightsComponent;
  let fixture: ComponentFixture<StudentAttendanceInsightsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StudentAttendanceInsightsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StudentAttendanceInsightsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
