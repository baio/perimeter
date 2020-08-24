// tslint:disable: no-unused-expression
describe('admins', () => {
    before(() => cy.reinitDb());
    before(() => cy.login());
    before(() => {
        cy.dataCy('env-btn').click();
        cy.dataCy('admins-menu-item').click();
    });

    it('roles should be open', () => {
        cy.url().should('include', 'admins');
        cy.rows().should('have.length', 1);
    });

    it('create', () => {
        cy.dataCy('create-item').click();
        cy.url().should('match', /\/domains\/\d+\/admins\/new/);

        cy.formField('userEmail')
            .type('adm@test.dev')
            .formSelectSingle('roleId', 0)
            .submitButton()
            .click();

        cy.rows().should('have.length', 2);
        cy.rows(0, 1).should('contain.text', 'DomainSuperAdmin');
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
