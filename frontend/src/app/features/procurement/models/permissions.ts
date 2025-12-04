import { PROCUREMENT_SUB_ROLES, ProcurementSubRole } from '../../../shared/models/user.model';

export const PROCUREMENT_MANAGERS_ONLY: ProcurementSubRole[] = ['ProcurementManager'];

export const PROCUREMENT_MANAGERS_AND_OFFICERS: ProcurementSubRole[] = [
  'ProcurementManager',
  'ProcurementOfficer',
];

export const PROCUREMENT_ALL_SUB_ROLES: ProcurementSubRole[] = [...PROCUREMENT_SUB_ROLES];
