// tslint:disable: no-unused-expression
describe('multi-tenant-admins', () => {
    const EMAIL = 'adm2@test.dev';
    const PASSWORD = 'pass123';

    before(() => cy.reinitDb());
    before(() => cy.visit('/tenants/1/admins'));

    it('roles should be open', () => {
        cy.url().should('match', /\/tenants\/\d+\/admins/);
        cy.rows().should('have.length', 1);
    });

    it('create', () => {
        cy.dataCy('create-item').click();
        cy.url().should('match', /\/tenants\/\d+\/admins\/new/);

        cy.formField('userEmail')
            .type(EMAIL)
            .formSelectSingle('roleId', 0)
            .submitButton()
            .click();

        cy.rows().should('have.length', 2);
        cy.rows(0, 1).should('contain.text', 'TenantSuperAdmin');
    });

    it('logout', () => {
        cy.dataCy('tool').click({ force: true });
        cy.dataCy('logout').click({ force: true });
        cy.url().should('match', /\/home/);
    });

    it('signup / login', () => {
        cy.visit('/home');
        cy.url().should('include', '/home');
        cy.dataCy('signup-button').click();
        cy.url().should('include', '/auth/register');

        cy.server();

        cy.route({ method: 'POST', url: '**/auth/sign-up' }).as('signup');

        cy.dataCy('email')
            .type(EMAIL)
            .dataCy('password')
            .type(PASSWORD)
            .dataCy('confirm-password')
            .type(PASSWORD)
            .dataCy('first-name')
            .type('adm2')
            .dataCy('last-name')
            .type('mr')
            .dataCy('agree')
            .click()
            .dataCy('submit')
            .click();

        cy.wait('@signup');

        cy.get('@signup').should((req: any) => {
            assert.isTrue(!!req.request.body.queryString);

            const qs = req.request.body.queryString;

            cy.url().should('include', '/auth/register-sent');

            const url = `/auth/register-confirm${qs}&token=${Cypress.env(
                'confirmSignupToken'
            )}`;

            cy.visit(url);

            cy.url().should('include', '/auth/login');

            cy.dataCy('email')
                .type(EMAIL)
                .dataCy('password')
                .type(PASSWORD)
                .dataCy('submit')
                .click();

            cy.url().should('include', '/login-cb');

            cy.url().should('not.include', '/login-cb');

            cy.window().then((win) => {
                assert.isTrue(!!win.localStorage.getItem('id_token'));
                assert.isTrue(!!win.localStorage.getItem('access_token'));
                assert.isTrue(!!win.localStorage.getItem('refresh_token'));
            });

            cy.url().should('include', '/tenants/1/domains');
        });
    });
});
