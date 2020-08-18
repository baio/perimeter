// tslint:disable: no-unused-expression
describe('tenant-admins', () => {
    before(() => cy.reinitDb());
    before(() => {
        cy.visit('tenant/admins');
    });

    it('roles should be open', () => {
        cy.url().should('include', 'tenant/admins');
        cy.rows().should('have.length', 1);
    });

    it('create', () => {
        cy.dataCy('create-item').click();
        cy.url().should('match', /\/tenant\/admins\/new/);

        cy.formField('userEmail')
            .type('adm@test.dev')
            .formSelectSingle('roleId', 1)
            .submitButton()
            .click();

        cy.rows().should('have.length', 2);
        cy.rows(0, 1).should('contain.text', 'TenantSuperAdmin');
    });

    it.skip('remove', () => {
        cy.rowCommand(0, 0)
            .click()
            .confirmYesButton()
            .click()
            .rows()
            .should('have.length', 0);
    });
});
