import { procurementSidebarMenu } from './menu';

describe('procurementSidebarMenu', () => {
  it('includes Roles & Permissions entry', () => {
    expect(procurementSidebarMenu).toEqual(
      jasmine.arrayContaining([
        jasmine.objectContaining({
          title: 'Roles & Permissions',
          path: '/procurement/roles-permissions',
        }),
      ])
    );
  });
});
