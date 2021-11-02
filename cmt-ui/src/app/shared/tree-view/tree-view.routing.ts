import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CmtTreeViewComponent } from './tree-view.component';

const routes: Routes = [
	{ path: '', redirectTo: 'treeview', pathMatch: 'full' },
	{ path: 'treeview', component: CmtTreeViewComponent }
];

export const routing: ModuleWithProviders = RouterModule.forChild(routes);
