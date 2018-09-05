# VTank Nav File Format

The basic file format is as follows:

```
<syntax> ::= <tag><type><node-count><node-list>

<tag> ::= "uTank2 NAV 1.2" <EOL>
<type> ::= <digit> <EOL>
<node-count> ::= <number>
<node-list> ::= <node> | <node> <node-list>
<node> ::= <node-type> <coords> <zero> <custom>
<custom> ::= <number> | <string> | <object-id> | <object-class> | <jump>


<node-type> ::= <number>
<object-id> ::= <number> <string>
<object-class> ::= <string> <string> <bool> <coords>
<jump> ::= <number> <bool> <number>

<coords> ::= <float> <float> <float>
<bool> ::= "True" <EOL> | "False" <EOL>
<float> ::= <number> | <digits> . <digits> <EOL>
<string> ::= <chars> <EOL>
<number> ::= <digits> <EOL>
<zero> ::= 0 <EOL>
```

type (1 circular, 2 linear, 3 target (follow), 4 once)

if type eq 3
  targetId
else
node count
[nodes]
node_type
x
y
z
0
switch node_type
case 0:
  done
case 1: use portal
  number (portal id?)
case 2: recall
  spell
case 3: wait
  delay
case 4: chat command
  command
case 5: vendor
  id
  name
case 6: portal2?
  portal name
  objclass
    true/false
    x
    y
    z
case 7: npc
  name
  objclass
    true/false
    x
    y
    z
case 8: checkpoint
  done
case 9: jump
  number (angle?)
  true/false (shift)
  number (delay?)
