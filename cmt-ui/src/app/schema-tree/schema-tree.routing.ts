import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CmtSchemaTreeComponent } from './schema-tree.component';
import { AuthGuard } from '../shared/auth.guard';
import { MarketGuard } from '../shared/market/market.guard';

const routes: Routes = [
	{
		path: 'schema-list',
		component: CmtSchemaTreeComponent,
		canActivate: [ AuthGuard, MarketGuard ]
	}
];

export const routing: ModuleWithProviders = RouterModule.forChild(routes);
