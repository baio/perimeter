// tslint:disable: no-unused-expression
describe('perms', () => {
    before(() => cy.reinitDb());
    before(() => {
        cy.visit('domains/pool');
        cy.dataCy('env-btn').click();
        cy.dataCy('users-menu-item').click();
    });

    it('roles should be open', () => {
        cy.url().should('include', 'users');
    });

});
