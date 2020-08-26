// tslint:disable: no-unused-expression
describe('login', () => {
    before(() => cy.reinitDb());

    before(() => cy.visit('/'));

    it('unauthorized user must be redirected to home page', () => {
        cy.url().should('include', 'home');
    });

    describe('when login', () => {
        before(() => cy.login());

        it('must be redirected to first available tenant', () => {
            cy.url().should('include', 'tenant/domains');
        });
    });
});
