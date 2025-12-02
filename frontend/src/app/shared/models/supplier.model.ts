import { PageRequest } from './pagination.model';

export type SupplierStatus = 'Approved' | 'Pending' | 'On Hold';

export interface Supplier {
  id: string;
  supplierCode: string;
  companyName: string;
  registrationNumber: string;
  primaryContactName: string;
  primaryContactEmail: string;
  primaryContactPhone: string;
  businessCategories: string[];
  companyAddress: string;
  website?: string | null;
  yearEstablished: number;
  numberOfEmployees: number;
  uploadedDocuments: string[];
  category: string;
  contactPerson: string;
  submissionDate: string;
  status: SupplierStatus;
  hasPortalAccess: boolean;
  portalUserEmail?: string | null;
}

export interface SupplierRequest {
  companyName: string;
  registrationNumber: string;
  primaryContactName: string;
  primaryContactEmail: string;
  primaryContactPhone: string;
  businessCategories: string[];
  companyAddress: string;
  website?: string;
  yearEstablished: number;
  numberOfEmployees: number;
  uploadedDocuments: string[];
  status: SupplierStatus;
  hasPortalAccess: boolean;
  portalUserEmail?: string;
}

export interface SupplierQueryRequest extends PageRequest {}
