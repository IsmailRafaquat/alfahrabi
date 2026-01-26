import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StaffAttendanceInsightsComponent } from './staff-attendance-insights.component';

describe('StaffAttendanceInsightsComponent', () => {
  let component: StaffAttendanceInsightsComponent;
  let fixture: ComponentFixture<StaffAttendanceInsightsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StaffAttendanceInsightsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StaffAttendanceInsightsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
