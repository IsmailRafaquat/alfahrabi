import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FeeStructureItemComponent } from './fee-structure-item.component';

describe('FeeStructureItemComponent', () => {
  let component: FeeStructureItemComponent;
  let fixture: ComponentFixture<FeeStructureItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [FeeStructureItemComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FeeStructureItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
