import '../support';
import { before } from 'mocha';

describe('auth/forgot-password page', () => {
    beforeEach(() => cy.visit('/auth/forgot-password'));

    it('Send button should be disabled by default', () =>
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

    it('All fields correct and agreement checked should enable send button', () =>
        cy
            .dataCy('email')
            .type('test@mail.dev')
            .dataCy('submit')
            .should('not.be.disabled'));

    it('When send reset-password fails form should display error', () => {
        cy.server();
        cy.route({
            method: 'POST',
            url: '**/auth/reset-password',
            status: 500,
            delay: 10,
            response: [],
        }).as('resetPassword');

        cy.dataCy('email')
            .type('test@mail.dev')
            .dataCy('submit')
            .click()
            .dataCy('submit-error')
            .should('be.visible');

        cy.wait('@resetPassword');
    });

    it('when reset-password success should be redirected to reset-password-sent page', () => {
        cy.server();

        cy.route({
            method: 'POST',
            url: '**/auth/reset-password',
            delay: 10,
            status: 200,
            response: {},
        }).as('resetPassword');

        cy.dataCy('email').type('test@mail.dev').dataCy('submit').click();

        cy.wait('@resetPassword');

        cy.url().should('include', '/auth/forgot-password-sent');
    });
    

    it('when user open reset-password with empty token he got error immediately', () => {
        cy.visit('/auth/forgot-password-reset');

        cy.dataCy('submit-error').should('be.visible');
    });

    describe('reset password page', () => {
        beforeEach(() => cy.visit('/auth/forgot-password-reset?token=123'));

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

        it('Confirm password field should show error when not the same', () =>
            cy
                .dataCy('password')
                .type('123456')
                .dataCy('confirm-password')
                .type('12345')
                .dataCy('confirm-password-not-match-error')
                .should('be.visible'));

        it('When password invalid should display error', () =>
            cy
                .dataCy('password')
                .type('123456')
                .dataCy('password-miss-upper-case-letter-error')
                .should('be.visible')
                .dataCy('password-miss-lower-case-letter-error')
                .should('be.visible')
                .dataCy('password-miss-special-char-error')
                .should('be.visible'));

        it('when user change password succesfully he should be redirected to login page', () => {
            cy.server();

            cy.route({
                method: 'POST',
                url: '**/auth/reset-password/confirm',
                delay: 100,
                status: 200,
                response: {},
            }).as('resetPasswordConfirm');

            cy.dataCy('password')
                .type('11$ASDfg')
                .dataCy('confirm-password')
                .type('11$ASDfg')
                .dataCy('submit')
                .click();

            cy.wait('@resetPasswordConfirm');
            cy.url().should(
                'include',
                '/auth/login;event=reset-password-success'
            );
            cy.dataCy('reset-password-success').should('be.visible');
        });
    });

});
