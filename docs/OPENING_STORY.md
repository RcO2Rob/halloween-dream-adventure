# Tutorial opening explanation

Chapter one now opens with a short in-world story card using the shared storybook typography:

> **Find your way home**
>
> You are trapped in a Halloween nightmare.
>
> 01 — Light the pumpkin lanterns to reveal hidden paths.
>
> 02 — Find the lost memory fragments deeper in the dream.
>
> 03 — Follow the light and escape the nightmare.

The second step describes the wider journey: the memory shrines are found in chapter two, not on the tutorial's first island. The footer reads **Move with WASD when you're ready**.

The explanation fades in over the tutorial's safe spawn and disappears automatically after sixteen active seconds. Movement or a gameplay action dismisses it sooner. The player can begin walking immediately; there is no confirmation screen or required click. Pausing or switching away preserves reading time. The normal HUD replaces the card when it closes.

Starting from the main menu or selecting chapter one shows the explanation. Retrying the current chapter skips it. A fresh journey shows it again, while chapters two through four never show the tutorial card. Existing automatic gate transitions retain their behavior. No saved scene or map layout is regenerated or modified.

Use `--smoke-test --opening-smoke` for the focused standalone check: four window sizes, idle spawn safety, pause, real controller movement, automatic timeout, fresh start, direct chapter selection and retries in all four chapters. Evidence is in `docs/qa/opening-story`. This review does not rerun the earlier full gameplay suite.
