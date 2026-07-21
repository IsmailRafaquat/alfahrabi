import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { BehaviorSubject, of } from 'rxjs';
import { ShopSettingService } from '../../proxy/shop-management/settings';
import { ShopSettingsComponent } from './shop-settings.component';

describe('ShopSettingsComponent', () => {
  let component: ShopSettingsComponent;
  let fixture: ComponentFixture<ShopSettingsComponent>;
  let response$: BehaviorSubject<any>;
  let service: jasmine.SpyObj<ShopSettingService>;

  beforeEach(async () => {
    response$ = new BehaviorSubject<any>({ shopDisplayName: 'Tenant Shop', currencyCode: 'PKR', currencySymbol: '₨', invoicePrefix: 'INV', purchaseOrderPrefix: 'PO' });
    service = jasmine.createSpyObj('ShopSettingService', ['get', 'createOrUpdate']);
    service.get.and.returnValue(response$);
    service.createOrUpdate.and.returnValue(of({} as any));
    await TestBed.configureTestingModule({
      declarations: [ShopSettingsComponent], imports: [ReactiveFormsModule],
      providers: [
        { provide: ShopSettingService, useValue: service },
        { provide: PermissionService, useValue: { getGrantedPolicy: () => true } },
        { provide: ToasterService, useValue: { success: jasmine.createSpy() } },
      ],
    }).overrideComponent(ShopSettingsComponent, { set: { template: '' } }).compileComponents();
    fixture = TestBed.createComponent(ShopSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads current settings', () => expect(component.form.value.shopDisplayName).toBe('Tenant Shop'));
  it('saves valid settings', () => { component.save(); expect(service.createOrUpdate).toHaveBeenCalled(); });
  it('blocks invalid input', () => { component.form.controls.shopDisplayName.setValue(''); component.save(); expect(service.createOrUpdate).not.toHaveBeenCalled(); });
  it('disables duplicate save while submitting', () => { component.submitting = true; component.save(); expect(service.createOrUpdate).not.toHaveBeenCalled(); });

  it('does not save without manage permission', async () => {
    TestBed.resetTestingModule();
    service.createOrUpdate.calls.reset();
    await TestBed.configureTestingModule({
      declarations: [ShopSettingsComponent], imports: [ReactiveFormsModule],
      providers: [
        { provide: ShopSettingService, useValue: service },
        { provide: PermissionService, useValue: { getGrantedPolicy: () => false } },
        { provide: ToasterService, useValue: { success: jasmine.createSpy() } },
      ],
    }).overrideComponent(ShopSettingsComponent, { set: { template: '' } }).compileComponents();
    const denied = TestBed.createComponent(ShopSettingsComponent).componentInstance;
    denied.ngOnInit(); denied.save();
    expect(service.createOrUpdate).not.toHaveBeenCalled();
  });
});
