import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TopbarLayoutComponent } from './topbar-layout.component';

describe('TopbarLayoutComponent', () => {
  let component: TopbarLayoutComponent;
  let fixture: ComponentFixture<TopbarLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TopbarLayoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TopbarLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
