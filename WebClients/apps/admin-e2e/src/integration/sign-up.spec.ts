import '../support';

const password = '#6VvR&^';

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

    it('Last name field should show error when empty s+', () =>
        cy
            .dataCy('agreement-modal-open')
            .click()
            .dataCy('agreement-modal')
            .should('be.visible')
            .dataCy('agreement-modal-close')
            .click()
            .should('not.exist'));

    it('All fields correct but agreement not read should disable Register button', () =>
        cy
            .dataCy('email')
            .type('test@mail.dev')
            .dataCy('password')
            .type(password)
            .dataCy('confirm-password')
            .type(password)
            .dataCy('first-name')
            .type('alice')
            .dataCy('last-name')
            .type('ms')
            .dataCy('submit')
            .should('be.disabled'));

    it('All fields correct and agreement checked should enable register button', () =>
        cy
            .dataCy('email')
            .type('test@mail.dev')
            .dataCy('password')
            .type(password)
            .dataCy('confirm-password')
            .type(password)
            .dataCy('first-name')
            .type('alice')
            .dataCy('last-name')
            .type('ms')
            .dataCy('agree')
            .click()
            .dataCy('submit')
            .should('not.be.disabled'));

    it('When send signup fails form should display error', () => {
        cy.server();
        cy.route({
            method: 'POST',
            url: '**/auth/sign-up',
            status: 500,
            delay: 10,
            response: [],
        }).as('signUp');

        cy.dataCy('email')
            .type('test@mail.dev')
            .dataCy('password')
            .type(password)
            .dataCy('confirm-password')
            .type(password)
            .dataCy('first-name')
            .type('alice')
            .dataCy('last-name')
            .type('ms')
            .dataCy('agree')
            .click()
            .dataCy('submit')
            .click()
            .dataCy('submit-error')
            .should('be.visible');

        cy.wait('@signUp');
    });

    it('When password invalid should display error', () =>
        cy
            .dataCy('email')
            .dataCy('password')
            .type('123456')
            .dataCy('password-miss-upper-case-letter-error')
            .should('be.visible')
            .dataCy('password-miss-lower-case-letter-error')
            .should('be.visible')
            .dataCy('password-miss-special-char-error')
            .should('be.visible'));

    it('server response for password field should be displayed correctly', () => {
        cy.server();

        cy.route({
            method: 'POST',
            url: '**/auth/sign-up',
            status: 400,
            delay: 10,
            response: {
                password: ['MISS_UPPER_LETTER', 'MISS_LOWER_LETTER'],
            },
        }).as('signUp');

        cy.dataCy('email')
            .type('max.putilov@gmail.com')
            .dataCy('password')
            .type(password)
            .dataCy('confirm-password')
            .type(password)
            .dataCy('first-name')
            .type('alice')
            .dataCy('last-name')
            .type('ms')
            .dataCy('agree')
            .click()
            .dataCy('submit')
            .click();

        cy.wait('@signUp');

        cy.dataCy('password-miss-upper-case-letter-error')
            .should('be.visible')
            .dataCy('password-miss-lower-case-letter-error')
            .should('be.visible')
            .dataCy('submit-error')
            .should('be.visible');
    });

    it('when signup success should be redirected to confirm sign-up sent page', () => {
        cy.server();

        cy.route({
            method: 'POST',
            url: '**/auth/sign-up',
            delay: 10,
            status: 200,
            response: {},
        }).as('signUp');

        cy.dataCy('email')
            .type('max.putilov@gmail.com')
            .dataCy('password')
            .type(password)
            .dataCy('confirm-password')
            .type(password)
            .dataCy('first-name')
            .type('alice')
            .dataCy('last-name')
            .type('ms')
            .dataCy('agree')
            .click()
            .dataCy('submit')
            .click();

        cy.wait('@signUp');

        cy.url().should('include', '/auth/register-sent');
    });

    it('when user open register-confirm page without token in query string error must be displayed', () => {
        cy.visit('/auth/register-confirm');
        cy.dataCy('signup-confirm-error').should('be.visible');
    });

    it('when user open register-confirm page with wrong token in query string error must be displayed', () => {
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

    it('when user open register-confirm page with correct token should be redirected to login page', () => {
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
        cy.url().should('include', '/auth/login;event=sign-up-confirm-success');
        cy.dataCy('sign-up-confirm-success').should('be.visible');
    });
});
