import '../support';

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

    it.skip('when user open register-confirm page without token in query string error must be displayed', () => {
        cy.visit('/auth/register-confirm');
        cy.dataCy('signup-confirm-error').should('be.visible');
    });

    it.skip('when user open register-confirm page with wrong token in query string error must be displayed', () => {
        cy.server();

        cy.route({
            method: 'POST',
            url: '**/auth/sign-up/confirm',
            status: 500,
            delay: 10,
            response: {},
        }).as('signUpConfirm');

        cy.visit('/auth/register-confirm?token=xxx');

        cy.wait('@signUpConfirm');
        cy.dataCy('signup-confirm-error').should('be.visible');
    });

    it.skip('when user open register-confirm page with correct token should be redirected to login page', () => {
        cy.server();

        cy.route({
            method: 'POST',
            url: '**/auth/sign-up/confirm',
            delay: 100,
            status: 200,
            response: {},
        }).as('signUpConfirm');

        cy.visit('/auth/register-confirm?token=xxx');

        cy.wait('@signUpConfirm');
        cy.route('/auth/login?event=sign-up-confirm-success');
        cy.dataCy('sign-up-confirm-success').should('be.visible');
    });
});
