import { clearLocalStorage } from './_setup';
import { EMAIL } from '../integration/_setup';
import { data } from 'cypress/types/jquery';

// tslint:disable: no-unused-expression
describe('reset-password', () => {
    before(() => cy.reinitDb());

    before(() => clearLocalStorage());

    before(() => {
        cy.visit('/');
        cy.dataCy('login-button').click();
    });

    describe('forgot password', () => {
        before(() => cy.dataCy('forgot-password').click());

        it('must be redirected forgot password page', () => {
            cy.url().should('include', 'auth/forgot-password');
        });

        describe('when send reset email', () => {
            before(() => {
                cy.dataCy('email').type(EMAIL);
                cy.dataCy('submit').click();
            });

            it('must be redirected to domain', () => {
                cy.url().should('include', 'auth/forgot-password-sent');
            });

            describe('when open reset password page', () => {
                before(() => {
                    const url = `/auth/forgot-password-reset?token=${Cypress.env(
                        'confirmSignupToken'
                    )}`;

                    cy.visit(url);
                });

                it('forgot password reset page should be open', () => {
                    cy.url().should('include', '/auth/forgot-password-reset');
                });

                describe('reset password must be success', () => {
                    before(() => {
                        cy.dataCy('password').type('Test123456');
                        cy.dataCy('confirm-password').type('Test123456');
                        cy.dataCy('submit').click();
                    });

                    it('should be redirected to success page', () => {
                        cy.url().should('include', 'auth/login');
                    });
                });
            });
        });
    });
});
