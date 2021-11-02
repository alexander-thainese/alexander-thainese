import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CmtUnauthorizedComponent } from './shared/unauthorized';
import { CmtMetadataElementsComponent } from './metadata-elements/metadata-elements.component';

export const routes: Routes = [
	{ path: '', redirectTo: 'metadata-elements', pathMatch: 'full' },
	{ path: 'metadata-elements', component: CmtMetadataElementsComponent },
	{ path: 'unauthorized', component: CmtUnauthorizedComponent },
	{ path: '**', redirectTo: 'login', pathMatch: 'full' }
];

export const routing: ModuleWithProviders = RouterModule.forRoot(routes, { useHash: true });
