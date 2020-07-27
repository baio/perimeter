import { getGreeting } from '../support/app.po';
import '../support';

describe('auth/register page', () => {
    beforeEach(() => cy.visit('/auth/register'));

    it('Register button should be disabled by default', () =>
        cy.dataCy('submit').should('be.disabled'));

    it('Email field should show error when empty s+', () =>
        cy
            .dataCy('email')
            .type(' ')
            .dataCy('email-required-error')
            .should('be.visible'));

    it('Email field should show error when empty', () =>
        cy
            .dataCy('email')
            .type(' ')
            .clear()
            .dataCy('email-required-error')
            .should('be.visible'));

    it('Email field should show error when is not email', () =>
        cy
            .dataCy('email')
            .type('not_email')
            .dataCy('email-invalid-error')
            .should('be.visible'));

    it('Password field should show error when empty s+', () =>
        cy
            .dataCy('password')
            .type(' ')
            .dataCy('password-required-error')
            .should('be.visible'));

    it('Password field should show error when less then 6 chars', () =>
        cy
            .dataCy('password')
            .type('12345')
            .dataCy('password-min-length-error')
            .should('be.visible'));

    it('Confirm password field should show error when not the smae', () =>
        cy
            .dataCy('password')
            .type('123456')
            .dataCy('confirm-password')
            .type('12345')
            .dataCy('confirm-password-not-match-error')
            .should('be.visible'));

    it('First name field should show error when empty s+', () =>
        cy
            .dataCy('first-name')
            .type(' ')
            .dataCy('first-name-required-error')
            .should('be.visible'));

    it('Last name field should show error when empty s+', () =>
        cy
            .dataCy('last-name')
            .type(' ')
            .dataCy('last-name-required-error')
            .should('be.visible'));
});
