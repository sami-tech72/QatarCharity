export interface SupplierProfile {
  companyName: string;
  registrationNumber: string;
  primaryContactName: string;
  primaryContactEmail: string;
  primaryContactPhone: string;
  companyAddress: string;
  website?: string | null;
  yearEstablished: number;
  numberOfEmployees: number;
}
