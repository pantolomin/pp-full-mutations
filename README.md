# pp-full-mutations

Current Phoenix Point release is limited to 2 body parts only allowed for mutation.

This "mod" allows all three body parts to receive a mutation.

# Code change

The code contains a constant "MAX_MUTATIONS" set to 2. Unfortunately it is impossible to only change the value of a constant because it is "inlined" in the code (not used at runtime).
So I had to settle to modify the method where it matters (fortunately a single one).

This also means that if they ever change the code for this method, the "mod" will fail.