import { clearLocalStorage } from './_setup';

// tslint:disable: no-unused-expression
describe('login-github', () => {
    before(() => cy.reinitDb());

    before(() => clearLocalStorage());

    before(() => cy.visit('/'));

    it('unauthorized user must be redirected to home page', () => {
        cy.url().should('include', 'home');
    });

    describe('when login with github button', () => {
        before(() => {
            cy.dataCy('login-button').click();
            cy.dataCy('submit-github').click();
        });

        it('must be redirected to github auth page', () => {
            cy.url().should('include', 'auth/login');
        });
    });
});
