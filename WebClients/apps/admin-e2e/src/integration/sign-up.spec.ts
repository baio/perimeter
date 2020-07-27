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
            .type('123456')
            .dataCy('confirm-password')
            .type('123456')
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
            .type('123456')
            .dataCy('confirm-password')
            .type('123456')
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
            .type('123456')
            .dataCy('confirm-password')
            .type('123456')
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

    it('when signup success should be redirected to signup congrats page', () => {
        expect(true).equals(false);
    });
});
