import { clearLocalStorage } from "./_setup";

// tslint:disable: no-unused-expression
describe('tenant-admins', () => {
    before(() => cy.reinitDb());
    before(() => clearLocalStorage());
    before(() => cy.login());
    before(() => cy.dataCy('admins-menu-item').click());

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
