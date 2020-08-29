import { clearLocalStorage } from './_setup';

describe('logout', () => {
    before(() => cy.reinitDb());

    before(() => clearLocalStorage());

    before(() => cy.login());

    it('should be tenants path', () =>
        cy.url().should('match', /\/tenants\/\d+\/domains/));

    it('logout success', () => {
        cy.dataCy('tool').click();
        cy.dataCy('logout').click();
        cy.url().should('match', /\/home/);
    });

    it('sso should be reset', () => {
        cy.dataCy('login-button').click();
        cy.url().should('match', /\/login/);
    });
});
