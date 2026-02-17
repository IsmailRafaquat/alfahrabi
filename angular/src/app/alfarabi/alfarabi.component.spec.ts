import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AlfarabiComponent } from './alfarabi.component';

describe('AlfarabiComponent', () => {
  let component: AlfarabiComponent;
  let fixture: ComponentFixture<AlfarabiComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AlfarabiComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AlfarabiComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
