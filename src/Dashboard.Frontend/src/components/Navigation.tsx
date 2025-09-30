import React, { useState, useEffect } from 'react';
import { Navbar, Nav, Container } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';
import moment from 'moment';

const Navigation: React.FC = () => {
  const [currentTime, setCurrentTime] = useState<string>('');

  useEffect(() => {
    const updateTime = () => {
      setCurrentTime(moment.utc().format('DD.MM.YYYY HH:mm:ss'));
    };

    updateTime();
    const interval = setInterval(updateTime, 1000);

    return () => clearInterval(interval);
  }, []);

  return (
    <Navbar bg="dark" variant="dark" expand="lg" className="mb-3">
      <Container fluid>
        <Navbar.Brand href="/">
          <img 
            src="/img/logo/Jobbr_Hell_FaviconArtboard 5 copy 2.png" 
            height="36" 
            className="me-2" 
            alt="Jobbr Logo"
          />
        </Navbar.Brand>
        
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
            <LinkContainer to="/dashboard">
              <Nav.Link>
                <i className="fas fa-tachometer-alt"></i>
                Dashboard
              </Nav.Link>
            </LinkContainer>
            
            <LinkContainer to="/jobs">
              <Nav.Link>
                <i className="far fa-calendar-alt"></i>
                Jobs
              </Nav.Link>
            </LinkContainer>
            
            <LinkContainer to="/runs">
              <Nav.Link>
                <i className="fas fa-flag-checkered"></i>
                Runs
              </Nav.Link>
            </LinkContainer>
          </Nav>
          
          <Nav>
            <Navbar.Text className="me-3">
              <i className="far fa-clock me-1"></i>
              {currentTime} UTC
            </Navbar.Text>
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default Navigation;