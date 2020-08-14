// tslint:disable: no-unused-expression
describe('apps', () => {
    before(() => cy.reinitDb());
    before(() => {
        cy.visit('domains/pool');
        cy.dataCy('env-btn').click();
    });

    it('app should be open', () => {
        cy.url().should('include', 'apps');
    });
});
