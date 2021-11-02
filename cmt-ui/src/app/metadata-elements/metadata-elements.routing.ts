import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CmtMetadataElementsComponent } from './metadata-elements.component';
import { AuthGuard, AuthIsAdmin } from '../shared/auth.guard';
import { MarketGuard } from '../shared/market/market.guard';

const routes: Routes = [
	{
		path: 'metadata-elements',
		component: CmtMetadataElementsComponent,
		canActivate: [ AuthGuard, MarketGuard ]
	}
];

export const routing: ModuleWithProviders = RouterModule.forChild(routes);
