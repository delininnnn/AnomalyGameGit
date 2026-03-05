Hey #speaker:delinn
Hi #speaker:milka
How are you? #speaker:delinn
I'm good. #speaker:milka
Have you checked out Jeffawe's Assets? #speaker:Ose
Jeffawe. Heard of him before. #speaker:delinn
Yeah. He makes Assets in Unity and Unreal. #speaker:milka
I think he just released his first some time ago right? #speaker:Ose
Yeah. A dialogue System. #speaker:milka
-> main

=== main ===
Nice. Have you tried it out before? #speaker:Ose
    +[Yes]
        -> Question

     +[No]
        Oh you definitely should. Check out his Asset Page on Unity. Jeffawe! #speaker:Ose
        -> Ending

=== Question ===
        Nice. Did you enjoy it? #speaker:Ose
        +[Yes] 
            -> Ending2
            
        +[No]
            Oh! You should tell him. Drop a comment on the Asset Page #speaker:Ose
            -> Ending

=== Ending ===
Alright I will #speaker:delinn
Ok. #speaker:Ose
Alright then. Bye #speaker:delinn
Bye #speaker:milka
Bye #speaker:milka
-> END

=== Ending2 ===
    Lovely #speaker:Ose
    -> END