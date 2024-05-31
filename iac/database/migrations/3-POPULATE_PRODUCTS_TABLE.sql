CREATE OR REPLACE FUNCTION generate_products(num_products INTEGER)
RETURNS VOID AS $$
DECLARE
    i INTEGER := 0;
BEGIN
    WHILE i < num_products LOOP
        INSERT INTO products (title, price)
        VALUES (
            'Product ' || (i + 1),
            1000.0
        );
        i := i + 1;
    END LOOP;
END;
$$ LANGUAGE plpgsql;

SELECT generate_products(10000000);