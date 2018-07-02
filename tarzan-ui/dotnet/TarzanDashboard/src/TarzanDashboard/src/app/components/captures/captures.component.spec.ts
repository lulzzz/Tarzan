import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CapturesComponent } from './captures.component';

describe('CapturesComponent', () => {
  let component: CapturesComponent;
  let fixture: ComponentFixture<CapturesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CapturesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CapturesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
