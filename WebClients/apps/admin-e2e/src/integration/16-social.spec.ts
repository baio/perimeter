// tslint:disable: no-unused-expression
describe('social', () => {
    const UPDATED_NAME = 'updated name';
    before(() => cy.reinitDb(true));

    before(() => {
        cy.visit('/domains/2/apps');
        cy.dataCy('social-menu-item').click();
    });

    it('social should be open', () => {
        cy.url().should('include', 'social');
    });

    describe('list', () => {
        it('rows count gt 0', () => {
            cy.rows().should('have.length.gt', 0);
        });
    });

    describe('enable', () => {
        it('enable 1st social connection', () => {
            cy.rows(0)
                .click()
                .formToggle('isEnabled')
                .click()
                .formField('clientId')
                .type('abcd')
                .formField('clientSecret')
                .type('123');

            cy.submitButton().click({ force: true });

            cy.get('.ant-drawer').should('not.exist');
        });

        it('update 1st social connection', () => {
            cy.rows(0).click().formField('clientId').clear().type('abcdef');

            cy.submitButton().click();

            cy.get('.ant-drawer').should('not.exist');
        });

        it('disable 1st social connection', () => {
            cy.rows(0).click().formToggle('isEnabled').click();

            cy.submitButton().click();

            cy.get('.ant-drawer').should('not.exist');
        });
    });

    describe.skip('change order', () => {
        before(() => {
            cy.rows(0)
                .click()
                .formToggle('isEnabled')
                .click()
                .formField('clientId')
                .type('abcd')
                .formField('clientSecret')
                .type('123');
            cy.submitButton().click({ force: true });
            cy.rows(1)
                .click()
                .formToggle('isEnabled')
                .click()
                .formField('clientId')
                .type('abcd')
                .formField('clientSecret')
                .type('123');
            cy.submitButton().click({ force: true });
        });

        it('drag 1st row to 2nd ', () => {
            cy.get('tr.ant-table-row:eq(1) td').should(
                'contain.text',
                'google'
            );
            cy.get('tr.ant-table-row:eq(2) td').should(
                'contain.text',
                'github'
            );

            cy.dataCy('ordering-mode-switch').click();

            cy.get(`tr.ant-table-row:eq(1)`)
                .trigger('mousedown', { which: 1 })
                .trigger('mousemove', { clientX: 0, clientY: 250 })
                .trigger('mouseup', { force: true });

            cy.get('tr.ant-table-row:eq(2) td').should(
                'contain.text',
                'google'
            );
            cy.get('tr.ant-table-row:eq(1) td').should(
                'contain.text',
                'github'
            );
        });
    });
});
