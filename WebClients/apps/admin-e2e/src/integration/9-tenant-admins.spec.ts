// tslint:disable: no-unused-expression
describe('tenant-admins', () => {
    before(() => cy.reinitDb());
    before(() => cy.login());
    before(() => cy.visit('/tenants/1/admins'));

    it('roles should be open', () => {
        cy.url().should('match', /\/tenants\/\d+\/admins/);
        cy.rows().should('have.length', 1);
    });

    it('create', () => {
        cy.dataCy('create-item').click();
        cy.url().should('match', /\/tenants\/\d+\/admins\/new/);

        cy.formField('userEmail')
            .type('adm@test.dev')
            .formSelectSingle('roleId', 1)
            .submitButton()
            .click();

        cy.rows().should('have.length', 2);
        cy.rows(0, 1).should('contain.text', 'TenantAdmin');
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
